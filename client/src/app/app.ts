import { Component } from "@angular/core";
// import { Home } from "./home/home";
import { RouterLink, RouterOutlet } from "@angular/router";

@Component({
  selector: "app-root",
  imports: [/*Home,*/ RouterLink, RouterOutlet],
  template: `
    <!-- <h1>Hello World</h1> -->
    <main>
      <a [routerLink]="['/']">
        <header class="brand-name">
          <img class="brand-logo" src="/public/logo.svg" alt="logo" aria-hidden="true" />
        </header>
      </a>
      <!-- Navigation link to the Todos page. routerLink is Angular's router-aware alternative
           to a plain <a href>. It updates the URL without a full page reload. -->
      <nav>
        <a [routerLink]="['/todos']">Todos</a>
      </nav>
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
