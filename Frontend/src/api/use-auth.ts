import { useState } from "react";
import { useAuth } from "../context/auth-context";

export const useRegister = () => {

    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const register = async (username: string, email: string, password: string) => {
        setIsLoading(true);
        setError(null);
        try {
            const response = await fetch("http://localhost:5264/auth/register", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ username, email, password }),
            });

            if (response.ok) {
                return true;
            }
            const error = await response.json();
            throw new Error(error.message);
        }
        catch (err: any) {
            setError(err.message);
            return false;
        }
        finally {
            setIsLoading(false);
        }
    }
    return { register, isLoading, error };
}

export const useLogin = () => {
    const { saveLogin } = useAuth();
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const login = async (username: string, password: string) => {
        setIsLoading(true);
        setError(null);

        try {
            const response = await fetch("http://localhost:5264/auth/login", {
                method: "POST",
                credentials: "include",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ username, password }),
            });


            if (!response.ok) {
                console.log(response);
                throw new Error("Failed to login. Check your credentials.");
            }


            const { accessToken, user } = await response.json();
            saveLogin(accessToken, user);

            // return data; // This would typically include your JWT token or session info
        } catch (err: any) {
            setError(err.message);
        } finally {
            setIsLoading(false);
        }
    };

    return { login, isLoading, error };
};

export const useTokenLogin = () => {
    const { saveLogin, accessToken } = useAuth();

    const tokenLogin = async () => {

        try {
            const response = await fetch('http://localhost:5264/auth/generate-access-token', {
                method: 'POST',
                credentials: 'include', // Include the cookie with the request
                body: accessToken ?? "",
            });


            if (!response.ok) {
                return
            }

            console.log("valid autologin response.");
            const { token, user } = await response.json();
            saveLogin(token, user);
            // return data; // This would typically include your JWT token or session info
        } catch (err: any) {
            console.error(err);
            return null;
        }
    };

    return { tokenLogin };
};



export const useLogout = () => {
    const { clearUser, accessToken, } = useAuth();

    const logout = async () => {

        try {
            const response = await fetch('http://localhost:5264/auth/logout', {
                method: 'POST',
                credentials: "include", // Include cookies for authentication
                headers: {
                    Authorization: `Bearer ${accessToken}`,
                    "Content-Type": "application/json", // Ensure JSON is correctly parsed
                },
            });


            if (!response.ok) {
                console.error("Unable to logout");
                return;
            }

            clearUser();

            // return data; // This would typically include your JWT token or session info
        } catch (err: any) {
            console.error(err);
            return null;
        }
    };

    return { logout };
};

export const useAuthFetch = () => {
    const { accessToken, } = useAuth();
    const { tokenLogin } = useTokenLogin();

    const authFetch = async (url: string, options: RequestInit = {}, retryCount = 0): Promise<Response> => {
        const MAX_RETRIES = 1;

        options.headers = {
            ...options.headers,
            Authorization: `Bearer ${accessToken}`,
        };
        options.credentials = "include";

        try {
            const response = await fetch(url, options);

            if (response.status === 401 && retryCount < MAX_RETRIES) {
                await tokenLogin(); // Refresh token logic
                return authFetch(url, options, retryCount + 1); // Retry with updated token
            }

            return response;
        } catch (error) {
            console.error("Error during fetch:", error);
            throw error;
        }
    };

    return authFetch;
};