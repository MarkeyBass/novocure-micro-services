import { AsyncPipe, NgClass } from "@angular/common";
import { Component, inject } from "@angular/core";
import { Observable } from "rxjs";
import { TodoItem } from "../todo";
import { TodoService } from "../todo.service";

@Component({
  selector: "app-todos",
  // AsyncPipe lets the template subscribe to Observables with the "| async" syntax.
  // NgClass lets us conditionally apply CSS classes based on component state.
  imports: [AsyncPipe, NgClass],
  template: `
    <section class="todos-page">
      <h2>All Todos</h2>

      <!--
        "| async" subscribes to the todos$ Observable here in the template.
        Once the HTTP response arrives, Angular re-renders with the emitted array.
        "as todoList" gives a local template variable to the unwrapped array.
      -->
      @if (todos$ | async; as todoList) {
        @if (todoList.length === 0) {
          <!-- Empty-state: shown when the API returns an empty array or catchError fires -->
          <p class="empty-state">No todos found.</p>
        } @else {
          <ul class="todo-list">
            @for (todo of todoList; track todo.id) {
              <!--
                NgClass toggles the "completed" CSS class when todo.isComplete is true.
                "track todo.id" tells Angular which item is which across re-renders —
                more efficient than tracking by index when items can be added/removed.
              -->
              <li [ngClass]="{ completed: todo.isComplete }">
                <span class="todo-id">#{{ todo.id }}</span>
                <span class="todo-name">{{ todo.name }}</span>
                <span class="todo-status">
                  {{ todo.isComplete ? "Done" : "Pending" }}
                </span>
              </li>
            }
          </ul>
        }
      } @else {
        <!-- Loading state: shown while the Observable has not emitted yet -->
        <p class="loading">Loading todos...</p>
      }
    </section>
  `,
  styles: [`
    .todos-page { padding: 1rem 2rem; }
    .todo-list { list-style: none; padding: 0; }
    .todo-list li {
      display: flex; gap: 1rem; align-items: center;
      padding: 0.5rem 0.75rem; border-bottom: 1px solid #eee;
    }
    /* "completed" class is added by NgClass when todo.isComplete === true */
    .todo-list li.completed { opacity: 0.5; text-decoration: line-through; }
    .todo-id { color: #888; font-size: 0.85rem; min-width: 2.5rem; }
    .todo-name { flex: 1; }
    .todo-status { font-size: 0.8rem; font-weight: bold; color: #555; }
    .loading, .empty-state { color: #888; font-style: italic; }
  `],
})
export class Todos {
  // inject() pulls the singleton TodoService from Angular's dependency injector.
  // private readonly todoService = inject(TodoService);

  constructor(private readonly todoService: TodoService) {}

  // todos$ is an Observable, not a plain array.
  // The "$" suffix is a convention meaning "this is a stream (Observable)".
  // The template's async pipe will subscribe to it and receive the emitted array.
  todos$: Observable<TodoItem[]> = this.todoService.getAllTodos();
}
