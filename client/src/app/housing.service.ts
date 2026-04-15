import { HttpClient } from "@angular/common/http";
import { Injectable, inject } from "@angular/core";
import { catchError, Observable, of, tap } from "rxjs";
import { HousingLocationInfo } from "./housinglocation";
import { HousingApplicationInfo } from "./housingapplication";

@Injectable({
  providedIn: 'root',
})
export class HousingService {
  readonly baseUrl = "https://angular.dev/assets/images/tutorials/common";
  // Inject Angular's HTTP client so this service uses the framework's data layer
  // instead of calling the browser fetch API directly.
  private readonly http = inject(HttpClient);

  // This JSON server endpoint is the source of truth for the tutorial data.
  // url = "http://localhost:3000/locations";
  urlLocations = "https://localhost:7152/api/locations"; // HousingApi
  urlApplications = "https://localhost:7152/api/applications"; // HousingApi

  getAllHousingLocations(): Observable<HousingLocationInfo[]> {

    // HttpClient already returns an Observable, which is the common Angular 16 style.
    // catchError keeps the template from crashing if the request fails.
    return this.http.get<HousingLocationInfo[]>(this.urlLocations).pipe(catchError(() => of([])));
  }

  getHousingLocationById(id: string): Observable<HousingLocationInfo | undefined> {
    console.log("getHousingLocationById: " + id);
    // This stays as an Observable too, so the component template can use the async pipe.
    // Returning undefined on error makes the missing-data state easier to handle in the UI.
    return this.http
      .get<HousingLocationInfo>(`${this.urlLocations}/${id}`)
      .pipe(catchError(() => of(undefined)));
  }

  // Returns all submitted housing applications as a single-emit Observable.
  // Adding this here (instead of a new service) because it belongs to the same
  // HousingApi domain — same base URL, same auth context, same responsibility boundary.
  getAllApplications(): Observable<HousingApplicationInfo[]> {
    return this.http
      .get<HousingApplicationInfo[]>(this.urlApplications)
      .pipe(
        tap((res) => console.log("Applications loaded:", res)),
        catchError((err) => {
          console.error("Failed to load applications:", err);
          // Emit an empty array so the template renders the empty-state instead of crashing.
          return of([]);
        }),
      );
  }

  submitApplication(
    housingId: string,
    firstName: string,
    lastName: string,
    email: string,
  ): Observable<HousingApplicationInfo | undefined> {
    // HttpClient returns an Observable (lazy stream).
    // This method returns a *pipeline* (via `.pipe(...)`) that the component can subscribe to.
    //
    // Flow:
    // - `post(...)` creates an Observable that will emit the HTTP response once.
    // - `tap(...)` runs a side effect (logging) without changing the emitted value.
    // - `catchError(...)` intercepts errors (network/4xx/5xx) and replaces them with `undefined`
    //   so the UI can handle "submit failed" as a normal value.
    return this.http
      .post<HousingApplicationInfo>(this.urlApplications, {
        housingId,
        firstName,
        lastName,
        email,
      })
      .pipe(
        // `tap` is for side effects only (logging, metrics). It does not transform the data.
        tap((res) => console.log("Application submitted:", res)),
        // `catchError` converts an error signal into a normal value, keeping the stream alive.
        catchError((err) => {
          console.error("Application submit failed:", err);
          // `of(undefined)` creates a new Observable that immediately emits `undefined` and completes.
          // That turns an "error" into a normal value so the UI can handle failure without the stream crashing.
          return of(undefined);
        }),
      );
  }
}
