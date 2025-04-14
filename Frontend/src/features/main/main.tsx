import "./main.css";
import { useAuth } from "../../context/auth-context";
import LoginForm from "../auth/login-form";

export const Main: React.FC = () => {
    const { user } = useAuth();
    if (user != null) { }

    return (
        <main className="centered-container">
            {user ?
                (<article>
                    Welcome {user.name}
                </article>) :
                (<article>
                    <LoginForm />
                </article>)
            }
        </main>
    );
};
