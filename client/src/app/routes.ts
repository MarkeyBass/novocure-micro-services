import { Routes } from "@angular/router";
import { Home } from "./home/home";
import { Details } from "./details/details";
// Import the new Todos page component so the router can instantiate it
import { Todos } from "./todos/todos";

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
];

export default routeConfig;
