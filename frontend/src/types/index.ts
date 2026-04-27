// Types for API responses
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  errors?: Record<string, string[]>;
}

// Add your domain types here
// Example:
// export interface Product {
//   id: number;
//   name: string;
//   price: number;
// }
