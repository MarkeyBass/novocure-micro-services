import { AsyncPipe } from "@angular/common";
import { Component } from "@angular/core";
import { Observable } from "rxjs";
import { HousingApplicationInfo } from "../housingapplication";
import { HousingService } from "../housing.service";
import { DatePipe } from "@angular/common";

@Component({
  selector: "app-applications",
  // AsyncPipe is needed so the template can use the "| async" syntax to subscribe
  // to the applications$ Observable without calling .subscribe() manually.
  imports: [AsyncPipe, DatePipe],
  template: `
    <section class="applications-page">
      <h2>Housing Applications</h2>

      <!--
        "| async" subscribes to applications$ here in the template.
        "as applicationList" unwraps the emitted array into a local template variable.
        While the Observable has not emitted yet, async returns null and the @else branch renders.
      -->
      @if (applications$ | async; as applicationList) {
        @if (applicationList.length === 0) {
          <!-- Empty state: shown when the API returns [] or catchError fires -->
          <p class="empty-state">No applications found.</p>
        } @else {
          <table class="applications-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>First Name</th>
                <th>Last Name</th>
                <th>Email</th>
                <th>Housing ID</th>
                <th>Submitted</th>
              </tr>
            </thead>
            <tbody>
              <!--
                "track application.id" gives Angular a stable identity per row
                so it can update only the rows that actually changed on re-render,
                instead of re-creating the entire list.
              -->
              @for (application of applicationList; track application.id) {
                <tr>
                  <td class="id-cell">{{ application.id }}</td>
                  <td>{{ application.firstName }}</td>
                  <td>{{ application.lastName }}</td>
                  <td>{{ application.email }}</td>
                  <td class="id-cell">{{ application.housingId }}</td>
                  <!--
                    The "date" pipe formats createdAt (ISO string from the API) into a
                    human-readable local date+time without writing any formatting code by hand.
                  -->
                  <td>{{ application.createdAt | date: "medium" }}</td>
                </tr>
              }
            </tbody>
          </table>
        }
      } @else {
        <!-- Loading state: rendered while the HTTP request is in flight -->
        <p class="loading">Loading applications...</p>
      }
    </section>
  `,
  styles: [`
    .applications-page { padding: 1rem 2rem; }
    .applications-table { width: 100%; border-collapse: collapse; font-size: 0.9rem; }
    .applications-table th {
      text-align: left; padding: 0.5rem 0.75rem;
      background: #f4f4f4; border-bottom: 2px solid #ddd;
    }
    .applications-table td { padding: 0.5rem 0.75rem; border-bottom: 1px solid #eee; }
    .applications-table tr:hover td { background: #fafafa; }
    /* ID columns are long hex strings — cap their width and clip overflow */
    .id-cell { max-width: 8rem; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; color: #888; font-size: 0.8rem; }
    .loading, .empty-state { color: #888; font-style: italic; }
  `],
})
export class Applications {
  // Constructor injection: Angular's DI system passes the singleton HousingService here.
  // This is equivalent to using inject(HousingService) — same result, classic style.
  constructor(private readonly housingService: HousingService) {}

  // applications$ holds the Observable returned by getAllApplications().
  // No HTTP request fires here — Observables are lazy. The request fires only when
  // the async pipe in the template subscribes (on component render).
  applications$: Observable<HousingApplicationInfo[]> =
    this.housingService.getAllApplications();
}
