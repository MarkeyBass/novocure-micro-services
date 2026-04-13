/*!
 * @license
 * Copyright Google LLC All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.dev/license
 */

import { provideHttpClient } from "@angular/common/http";
import { ApplicationConfig } from "@angular/core";

export const appConfig: ApplicationConfig = {
  providers: [
    // Registers Angular HttpClient once so services can inject it anywhere in the app.
    provideHttpClient(),
  ],
};
