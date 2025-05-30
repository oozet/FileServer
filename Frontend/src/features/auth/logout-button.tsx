import { useLogout } from "../../api/use-auth";

const LogoutButton: React.FC = () => {
    const { logout } = useLogout();

    const handleLogout = async () => {
        await logout();
    };

    return <><button onClick={handleLogout}>Logout</button></>;
};

export default LogoutButton;
