import { useAuth } from "../../context/auth-context";
import LoginComponent from "../auth/login-component";
import LogoutButton from "../auth/logout-button";
import "./header.css";

export const Header: React.FC = () => {

    const { user } = useAuth();

    return (
        <header className="centered-container">
            <h1>Title</h1>
            {user ? (
                <>
                    <LogoutButton />
                </>
            ) : (
                <LoginComponent />
            )}
        </header>
    );
};
