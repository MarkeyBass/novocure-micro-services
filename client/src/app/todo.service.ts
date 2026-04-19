import { HttpClient } from "@angular/common/http";
import { Injectable, inject } from "@angular/core";
import { catchError, Observable, of, tap } from "rxjs";
import { TodoItem } from "./todo";

@Injectable({
  // 'root' means Angular creates a single shared instance for the whole app (singleton).
  // Any component or service can inject it without re-declaring it.
  providedIn: "root",
})
export class TodoService {
  // Use a relative URL so the Angular dev proxy can route to either
  // the host-run API or the docker-compose API without code changes.
  private readonly todoApiUrl = "/api/TodoItems";

  // inject() is the modern Angular alternative to constructor injection.
  // HttpClient is already registered in app.config.ts via provideHttpClient().
  // private readonly http = inject(HttpClient);

  // DI via constructor injection
  constructor(private readonly http: HttpClient) {}

  // Returns an Observable stream that will emit exactly one value: the array of todos.
  // Using Observable (instead of a plain Promise) lets the template subscribe with the
  // async pipe, which automatically unsubscribes when the component is destroyed.
  getAllTodos(): Observable<TodoItem[]> {
    return this.http
      .get<TodoItem[]>(this.todoApiUrl)
      .pipe(
        tap((res) => console.log("Todos loaded:", res)),
        // catchError intercepts HTTP errors (network down, 4xx, 5xx) and replaces
        // the error signal with an empty array so the template stays functional.
        catchError((err) => {
          console.error("Failed to load todos:", err);
          return of([]);
        }),
      );
  }
}
