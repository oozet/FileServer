import React, { useState } from "react";
import { useRegister } from "../../api/use-auth";

interface RegisterFormProps {
    setShowRegisterForm: React.Dispatch<React.SetStateAction<boolean>>;
}
const RegisterForm: React.FC<RegisterFormProps> = ({ setShowRegisterForm }) => {
    const [username, setUsername] = useState("");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const { register, isLoading, error } = useRegister();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        const success = await register(username, email, password);
        if (success) setShowRegisterForm(false);
    };

    return (
        <div style={{ maxWidth: "400px", margin: "0 auto" }}>
            <h2>Register</h2>
            <form onSubmit={handleSubmit}>
                <label htmlFor="username">Username</label>
                <input
                    id="username"
                    type="text"
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                    required
                />
                <label htmlFor="email">Email</label>
                <input
                    id="email"
                    type="email"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
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
                    {isLoading ? "Registering..." : "Register"}
                </button>
                {error && <p style={{ color: "red" }}>{error}</p>}
            </form>
        </div>
    );
};

export default RegisterForm;
