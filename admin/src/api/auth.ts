import { apiRequest } from "./client";

export interface AuthenticatedUser {
  email: string;
  displayName: string;
  role: string;
}

interface LoginResponse {
  accessToken: string;
  user: AuthenticatedUser;
}

export function login(email: string, password: string): Promise<LoginResponse> {
  return apiRequest<LoginResponse>("/api/auth/login", {
    method: "POST",
    body: { email, password },
  });
}
