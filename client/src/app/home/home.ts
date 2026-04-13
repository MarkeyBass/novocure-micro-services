import { AsyncPipe } from "@angular/common";
import { Component, inject } from "@angular/core";
import { map, Observable, shareReplay } from "rxjs";

import { HousingLocation } from "../housing-location/housing-location";
import { HousingLocationInfo } from "../housinglocation";
import { HousingService } from "../housing.service";

@Component({
  selector: "app-home",
  imports: [HousingLocation, AsyncPipe],
  template: `
    <section>
      <form>
        <div class="form-container">
          <input type="text" placeholder="Filter by city" #filter />
          <button class="primary button" type="button" (click)="filterdeResults(filter.value)">
            Search
          </button>
        </div>
      </form>
    </section>
    <section class="results">
      <!-- "filteredLocationList$" is an Observable (a stream), not a plain array.
      The "async" pipe subscribes to that stream, waits for it to emit the list,
      and then exposes the emitted array to the template as "filteredLocationList". -->
      @if (filteredLocationList$ | async; as filteredLocationList) {
        @for (housingLocation of filteredLocationList; track $index) {
          <app-housing-location [housingLocation]="housingLocation" />
        }
      }
    </section>
  `,
  styleUrls: ["./home.css"],
})
export class Home {
  housingService = inject(HousingService);
  // Keep the original list as an Observable so the template can subscribe with async.
  // shareReplay(1) caches the first HTTP response and avoids a new request on every search.
  // If the data changes often and must always be fresh, 
  //    then caching with shareReplay(1) may be the wrong choice unless you add a refresh strategy.
  housingLocationList$: Observable<HousingLocationInfo[]> = this.housingService
    .getAllHousingLocations()
    .pipe(shareReplay(1));

  // Start by showing the full list before the user types a filter.
  filteredLocationList$: Observable<HousingLocationInfo[]> = this.housingLocationList$;

  filterdeResults(text: string) {
    console.log(text);
    // Each click builds a derived stream from the cached list instead of mutating arrays by hand.
    this.filteredLocationList$ = this.housingLocationList$.pipe(
      map((housingLocationList) => {
        if (!text) {
          return housingLocationList;
        }

        return housingLocationList.filter((housingLocation) =>
          housingLocation.city.toLowerCase().includes(text.toLowerCase()),
        );
      }),
    );
  }
}
