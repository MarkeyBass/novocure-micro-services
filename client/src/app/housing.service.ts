import { HttpClient } from "@angular/common/http";
import { Injectable, inject } from "@angular/core";
import { catchError, Observable, of } from "rxjs";
import { HousingLocationInfo } from "./housinglocation";

@Injectable({
  providedIn: 'root',
})
export class HousingService {
  readonly baseUrl = "https://angular.dev/assets/images/tutorials/common";
  // Inject Angular's HTTP client so this service uses the framework's data layer
  // instead of calling the browser fetch API directly.
  private readonly http = inject(HttpClient);

  // This JSON server endpoint is the source of truth for the tutorial data.
  url = "http://localhost:3000/locations";

  getAllHousingLocations(): Observable<HousingLocationInfo[]> {

    // HttpClient already returns an Observable, which is the common Angular 16 style.
    // catchError keeps the template from crashing if the request fails.
    return this.http.get<HousingLocationInfo[]>(this.url).pipe(catchError(() => of([])));
  }

  getHousingLocationById(id: number): Observable<HousingLocationInfo | undefined> {

    // This stays as an Observable too, so the component template can use the async pipe.
    // Returning undefined on error makes the missing-data state easier to handle in the UI.
    return this.http
      .get<HousingLocationInfo>(`${this.url}/${id}`)
      .pipe(catchError(() => of(undefined)));
  }

  submitApplication(firstName: string, lastName: string, email: string) {
    console.log(
      `Homes application received: firstName: ${firstName}, lastName: ${lastName}, email: ${email}.`,
    );
  }
}
