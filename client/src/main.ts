/*
 *  Protractor support is deprecated in Angular.
 *  Protractor is used in this example for compatibility with Angular documentation tools.
 */
import { bootstrapApplication, provideProtractorTestingSupport } from "@angular/platform-browser";
import { App } from "./app/app";

// enabling routing inside the application
import { provideRouter } from "@angular/router";
import routeConfig from "./app/routes";

bootstrapApplication(App, {
  providers: [
    provideProtractorTestingSupport(), 
    provideRouter(routeConfig)
  ],
}).catch((err) => console.error(err));
