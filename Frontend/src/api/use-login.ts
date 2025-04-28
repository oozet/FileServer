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
            return null;
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