import { Routes } from "@angular/router";
import { Home } from "./home/home";
import { Details } from "./details/details";
// Import the new Todos page component so the router can instantiate it
import { Todos } from "./todos/todos";
// Import the Applications page component
import { Applications } from "./applications/applications";

const routeConfig: Routes = [
  {
    path: "",
    component: Home,
    title: "Home page",
  },
  {
    path: "details/:id",
    component: Details,
    title: "Home details",
  },
  // New route: navigating to /todos renders the Todos component
  {
    path: "todos",
    component: Todos,
    title: "Todos",
  },
  // New route: navigating to /applications renders the Applications component
  {
    path: "applications",
    component: Applications,
    title: "Housing Applications",
  },
];

export default routeConfig;
