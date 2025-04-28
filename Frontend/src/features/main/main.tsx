import "./main.css";
import { useAuth } from "../../context/auth-context";
import DirectoryComponent from "../directories/directory-component";

export const Main: React.FC = () => {
    const { user } = useAuth();
    if (user) {
        console.log(user);
    }
    return (
        <main className="centered-container">
            {user ?
                (<article>
                    Welcome {user.name}
                    <DirectoryComponent />
                </article>) :
                (<article>
                </article>)
            }
        </main>
    );
};
