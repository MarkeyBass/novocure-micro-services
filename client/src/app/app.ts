import { Component } from "@angular/core";
// import { Home } from "./home/home";
import { RouterLink, RouterLinkActive, RouterOutlet } from "@angular/router";

@Component({
  selector: "app-root",
  imports: [/*Home,*/ RouterLink, RouterLinkActive, RouterOutlet],
  template: `
    <!-- Old reference code -->
    <!-- <h1>Hello World</h1> -->
    <!-- <app-home /> -->
    <main>
      <!-- Top bar: logo on the left, nav links on the right, all in one flex row -->
      <header class="topbar">
        <a class="brand" [routerLink]="['/']">
          <img class="brand-logo" src="/public/logo.svg" alt="logo" aria-hidden="true" />
          <span class="brand-name">Homes</span>
        </a>
        <!-- Navigation links. routerLink updates the URL without a full page reload.
             RouterLinkActive adds the "active" class when the URL matches the link. -->
        <nav class="nav-links">
          <a [routerLink]="['/']" routerLinkActive="active" [routerLinkActiveOptions]="{ exact: true }">Home</a>
          <a [routerLink]="['/todos']" routerLinkActive="active">Todos</a>
          <a [routerLink]="['/applications']" routerLinkActive="active">Applications</a>
        </nav>
      </header>
      <section class="content">
        <!-- <app-home /> -->
        <!-- router outlet is a placeholder for the child routes -->
        <router-outlet />
      </section>
    </main>
  `,
  styleUrls: ["./app.css"],
})
export class App {
  title = "Homes";
}
