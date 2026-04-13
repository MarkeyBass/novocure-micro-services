import { AsyncPipe } from "@angular/common";
import { Component, effect, inject } from "@angular/core";
import { HousingService } from "../housing.service";
import { HousingLocationInfo } from "../housinglocation";
import { ActivatedRoute } from "@angular/router";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { Observable } from "rxjs";

@Component({
  selector: "app-details",
  imports: [ReactiveFormsModule, AsyncPipe],
  template: `
    <!-- housingLocation$ is an Observable (a stream), not a plain object.
    The "async" pipe subscribes to that stream, waits for it to emit the object,
    and then exposes the emitted object to the template as "housingLocation". -->
    @if (housingLocation$ | async; as housingLocation) {
      <article>
        <img
          class="listing-photo"
          [src]="housingLocation?.photo"
          alt="Exterior photo of {{ housingLocation?.name }}"
          crossorigin
        />
        <section class="listing-description">
          <h2 class="listing-heading">{{ housingLocation?.name }}</h2>
          <p class="listing-location">{{ housingLocation?.city }}, {{ housingLocation?.state }}</p>
        </section>
        <section class="listing-features">
          <h2 class="section-heading">About this housing location</h2>
          <ul>
            <li>Units available: {{ housingLocation?.availableUnits }}</li>
            <li>Does this location have wifi: {{ housingLocation?.wifi }}</li>
            <li>Does this location have laundry: {{ housingLocation?.laundry }}</li>
          </ul>
        </section>
        <section class="listing-apply">
          <h2 class="section-heading">Apply now to live here</h2>
          <form [formGroup]="applyForm" (ngSubmit)="submitApplication()">
            <label for="first-name">First Name</label>
            <input
              id="first-name"
              type="text"
              formControlName="firstName"
              aria-describedby="first-name-error"
            />
            @if (firstName.invalid && (firstName.dirty || firstName.touched)) {
              <p id="first-name-error" class="field-error" role="alert">
                @if (firstName.hasError("required")) {
                  First name is required.
                }
              </p>
            }
            <label for="last-name">Last Name</label>
            <input
              id="last-name"
              type="text"
              formControlName="lastName"
              aria-describedby="last-name-error"
            />
            @if (lastName.invalid && (lastName.dirty || lastName.touched)) {
              <p id="last-name-error" class="field-error" role="alert">
                @if (lastName.hasError("required")) {
                  Last name is required.
                }
              </p>
            }
            <label for="email">Email</label>
            <input id="email" type="email" formControlName="email" aria-describedby="email-error" />
            @if (email.invalid && (email.dirty || email.touched)) {
              <p id="email-error" class="field-error" role="alert">
                @if (email.hasError("required")) {
                  Email is required.
                }
                @if (email.hasError("email")) {
                  Enter a valid email address.
                }
              </p>
            }
            <button type="submit" class="primary button" [disabled]="applyForm.invalid">
              Apply now
            </button>
          </form>
        </section>
      </article>
    }
  `,
  styles: ``,
  styleUrls: ["./details.css"],
})
export class Details {
  readonly housingService = inject(HousingService);
  readonly route = inject(ActivatedRoute);
  housingLocationId: number = Number(this.route.snapshot.params["id"]);

  // The template subscribes to this Observable with the async pipe.
  // That removes the need for Promise.then(...) and markForCheck().
  housingLocation$: Observable<HousingLocationInfo | undefined> =
    this.housingService.getHousingLocationById(this.housingLocationId);

  constructor() {
    effect(() => {
      // log the observable
      // console.log("housingLocation:", this.housingLocation$);
      // subscribe to the observable and log the value after it is emitted
      this.housingLocation$.subscribe((housingLocation) => {
        console.log("housingLocation:", housingLocation);
      });
      console.log("housingLocationId:", this.housingLocationId);
    });
  }

  // Reactive form for applying to a housing location
  applyForm = new FormGroup({
    firstName: new FormControl("", [Validators.required]),
    lastName: new FormControl("", Validators.required),
    email: new FormControl("", [Validators.required, Validators.email]),
  });

  // getters for the form controls
  get firstName() {
    return this.applyForm.controls.firstName;
  }

  get lastName() {
    return this.applyForm.controls.lastName;
  }

  get email() {
    return this.applyForm.controls.email;
  }

  submitApplication() {
    if (this.applyForm.invalid) {
      this.applyForm.markAllAsTouched();
      return;
    }
    this.housingService.submitApplication(
      this.applyForm.value.firstName ?? "",
      this.applyForm.value.lastName ?? "",
      this.applyForm.value.email ?? "",
    );
  }
}
