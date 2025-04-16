import React, { useState } from "react";
import { useTokenLogin } from "../../api/use-login";
import LoginForm from "./login-form";
import "./login-component.css";

const LoginComponent: React.FC = () => {
    const { tokenLogin } = useTokenLogin();
    const [showLoginForm, setShowLoginForm] = useState(false);

    const handleCloseForm = () => {
        setShowLoginForm(false);
    };


    const handleLogin = async () => {
        try {
            setShowLoginForm(true);
            await tokenLogin(); // Attempt token-based login
            // If successful, proceed (e.g., redirect or show a message)
        } catch (err) {
            console.error("Token login failed:", err);
        };
    };


    return (
        <div>
            <button onClick={handleLogin}>
                Login
            </button>
            {showLoginForm && (
                <>
                    <div className="modal-overlay" onClick={handleCloseForm}></div>
                    <div className="login-form-window">
                        <LoginForm />
                    </div>
                </>
            )}
        </div>
    );
};

export default LoginComponent;
