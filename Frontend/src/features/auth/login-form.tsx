import React, { useState } from "react";
import { useLogin } from "../../api/use-login";

const LoginForm: React.FC = () => {
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const { login, isLoading, error } = useLogin();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        await login(username, password);
    };

    return (
        <div style={{ maxWidth: "400px", margin: "0 auto" }}>
            <h2>Login</h2>
            <form onSubmit={handleSubmit}>
                <label htmlFor="username">Username or Email</label>
                <input
                    id="username"
                    type="text"
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                    required
                />
                <label>Password</label>
                <input
                    type="password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    required
                />
                <button type="submit" disabled={isLoading}>
                    {isLoading ? "Logging in..." : "Login"}
                </button>
                {error && <p style={{ color: "red" }}>{error}</p>}
            </form>
        </div>
    );
};

export default LoginForm;
