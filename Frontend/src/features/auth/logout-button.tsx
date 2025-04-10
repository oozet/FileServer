import React, { useContext } from "react";
import { AuthContext } from "./auth-context";

const LogoutButton: React.FC = () => {

    const authContext = useContext(AuthContext);

    const { logout, user } = authContext;
    const handleLogout = () => {
        logout(); // Clears the context
    };

    return <><div>{user?.name}</div><button onClick={handleLogout}>Logout</button></>;
};
export default LogoutButton;
