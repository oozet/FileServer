import React, { useState } from "react";
import RegisterForm from "./register-form";
import "./auth-window-component.css";

const RegisterComponent: React.FC = () => {
    const [showRegisterForm, setShowRegisterForm] = useState(false);

    const handleCloseForm = () => {
        setShowRegisterForm(false);
    };

    const handleRegister = async () => {
        try {
            setShowRegisterForm(true);
        } catch (err) {
            console.error("Token login failed:", err);
        };
    };


    return (
        <div>
            <button onClick={handleRegister}>
                Register
            </button>
            {showRegisterForm && (
                <>
                    <div className="modal-overlay" onClick={handleCloseForm}></div>
                    <div className="auth-form-window">
                        <RegisterForm setShowRegisterForm={setShowRegisterForm} />
                    </div>
                </>
            )}
        </div>
    );
};

export default RegisterComponent;
