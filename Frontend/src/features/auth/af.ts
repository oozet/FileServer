const refreshToken = async () => {
    const response = await fetch("https://localhost:5001/auth/token/refresh", {
        method: "POST",
        credentials: "include", // Include HTTP-only cookies
    });

    if (response.ok) {
        const data = await response.json();
        sessionStorage.setItem("accessToken", data.accessToken); // Update access token
    } else {
        console.error("Failed to refresh token");
    }
};

export default refreshToken;

import { useContext } from "react";
import AuthContext from "./AuthContext";

function useFetchWithAuth(url, options) {
    const { token, setToken, logout } = useContext(AuthContext);

    const fetchWithAuth = async () => {
        const response = await fetch(url, {
            ...options,
            headers: {
                ...options.headers,
                Authorization: `Bearer ${token}`,
            },j
        });

        if (response.status === 401) {
            // Handle token expiration
            logout(); // Or refresh the token if possible
        }

        return response;
    };

    return fetchWithAuth;
}


const fetchData = async () => {
    const accessToken = sessionStorage.getItem("accessToken");

    const response = await fetch("https://localhost:5001/api/data", {
        method: "GET",
        headers: { Authorization: `Bearer ${accessToken}` },
    });

    if (response.ok) {
        const data = await response.json();
        console.log(data);
    } else {
        console.error("Failed to fetch data");
    }
};


const login = async () => {
    const response = await fetch("https://localhost:5001/api/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username, password }),
    });

    if (response.ok) {
        const data = await response.json();
        sessionStorage.setItem("accessToken", data.accessToken); // Store access token
    } else {
        console.error("Login failed");
    }
};
