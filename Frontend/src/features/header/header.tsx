import LogoutButton from "../auth/logout-button";
import "./header.css";

export const Header: React.FC = () => {

    return (
        <header className="centered-container">
            <h1>Title</h1>

            <LogoutButton />
        </header>
    );
};
