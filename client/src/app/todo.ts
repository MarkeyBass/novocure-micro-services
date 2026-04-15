// Mirrors the TodoItemDTO returned by TodoApi's GET /api/TodoItems endpoint.
// Using an interface (not a class) because we only need the shape for type-checking —
// no constructor logic or methods are needed.
export interface TodoItem {
  id: number;
  name: string;
  isComplete: boolean;
}
