import { useState } from "react";
import { useAuth } from "../context/auth-context";

interface LoginResponse {
    token: string;
}

export const useLogin = () => {
    const { saveLogin } = useAuth();
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const login = async (username: string, password: string) => {
        setIsLoading(true);
        setError(null);

        try {
            const response = await fetch("http://localhost:5264/login", {
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


            const { data } = await response.json();
            saveLogin(data.accessToken, data.user);
            // return data; // This would typically include your JWT token or session info
        } catch (err: any) {
            setError(err.message);
            return null;
        } finally {
            setIsLoading(false);
        }
    };

    return { login, isLoading, error };
};

export const useTokenLogin = () => {
    const { saveLogin, authToken } = useAuth();

    const tokenLogin = async () => {

        try {
            const response = await fetch('http://localhost:5264/get-user', {
                method: 'POST',
                credentials: 'include', // Include the cookie with the request
                body: authToken ?? "",
            });

            console.log(response);
            if (!response.ok) {
                console.log('Refresh token is invalid or expired');
                return
            }


            const { data } = await response.json();
            saveLogin(data.accessToken, data.user);
            // return data; // This would typically include your JWT token or session info
        } catch (err: any) {
            console.error(err);
            return null;
        }
    };

    return { tokenLogin };
};