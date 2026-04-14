# Housing API Migration Steps

This file is the execution checklist for the first housing microservice migration.

We will work in small steps:

1. Implement one step.
2. Review the code changes together.
3. Test that step.
4. Only then continue to the next step.

## Step 1: Convert `HousingApi` from bookstore CRUD to housing CRUD

Goal:
- Replace book-specific model, service, controller, and config with housing-specific equivalents.
- Keep full CRUD behavior.
- Use MongoDB `ObjectId` string ids.

Expected result:
- `HousingApi` exposes CRUD endpoints for housing locations.
- The API shape matches the Angular housing domain.

Review before moving on:
- Confirm route names.
- Confirm housing model fields.
- Confirm Mongo collection naming.

Test checklist:
- Run `HousingApi`.
- Verify `GET all`, `GET by id`, `POST`, `PUT`, and `DELETE`.
- Verify Swagger/OpenAPI loads.

Status:
- Implemented, pending review and testing

## Step 2: Add housing application submissions collection

Goal:
- Add a second entity for submitted housing applications.
- Store:
  - `firstName`
  - `lastName`
  - `email`
  - `createdAt`
  - `housingId`

Expected result:
- New Mongo collection for applications.
- New model, DTOs, service, and controller.
- Endpoints for creating submissions and listing all submissions.

Review before moving on:
- Confirm payload shape.
- Confirm `createdAt` is assigned server-side.
- Confirm `housingId` format and validation.

Test checklist:
- Submit one application through the API.
- Verify `GET all` returns saved submissions.
- Verify invalid payload handling.

Status:
- Implemented, pending review and testing

## Step 3: Switch Angular housing reads to `HousingApi`

Goal:
- Replace JSON server reads with real API reads.
- Update Angular housing id type from numeric to string.

Expected result:
- Home page loads data from `HousingApi`.
- Details page loads by Mongo `ObjectId`.

Review before moving on:
- Confirm `HousingLocationInfo` contract.
- Confirm route parameter handling.
- Confirm API base URL choice.

Test checklist:
- Load housing list in the browser.
- Open details for one location.
- Verify empty/error handling still works.

Status:
- Pending

## Step 4: Wire Angular submit form to the applications endpoint

Goal:
- Replace the current console-only submit with a real POST request.

Expected result:
- The details form sends application data to `HousingApi`.
- Payload includes `housingId`.

Review before moving on:
- Confirm submit payload.
- Confirm success/failure behavior in the UI.

Test checklist:
- Submit the form from the UI.
- Verify the document is stored in MongoDB.
- Verify `GET all` shows the new submission.

Status:
- Pending

## Step 5: Cleanup and verify local workflow

Goal:
- Remove JSON-server dependency from the housing flow.
- Confirm local run instructions are correct.

Expected result:
- Housing frontend and backend work together without `db.json`.
- Temporary mock-data instructions are no longer needed for this feature flow.

Review before moving on:
- Confirm which local commands remain.
- Confirm whether `db.json` should stay for reference only or be retired.

Test checklist:
- Start API and client together.
- Verify list, details, and submit flow end-to-end.

Status:
- Pending

## Future Step: RabbitMQ integration

Not part of this pass.

Planned later:
- After saving a housing application, publish an event.
- A consumer in `TodoApi` will create or update a todo item based on that event.
