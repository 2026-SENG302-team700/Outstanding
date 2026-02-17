import { resolve } from '$app/paths';

let csrfToken: string | null = null;

export async function getCsrfToken(): Promise<string> {
    if (!csrfToken) {
        // Use 'as any' to bypass the "Route not found" error for your C# API
        const response = await fetch(resolve(`/api/csrf-token` as any), {
            credentials: 'include'
        });
        const data = await response.json();
        csrfToken = data.token;
    }
    return csrfToken as string;
}

export async function fetchWithCsrf(url: string, options: RequestInit = {}) {
    const token = await getCsrfToken();
    return fetch(url, {
        ...options,
        credentials: 'include',
        headers: {
            ...options.headers,
            'X-CSRF-TOKEN': token
        }
    });
}
