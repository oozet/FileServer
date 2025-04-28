import React, { createContext, useState, ReactNode, useEffect, useContext } from "react";

// Define types for AuthContext and its values
interface AuthContextType {
  accessToken: string | null;
  user: User | null;
  saveLogin: (accessToken: string, user: User) => void;
  clearUser: () => void;
}

const defaultAuthContext: AuthContextType = {
  accessToken: null,
  user: null,
  saveLogin: () => { },
  clearUser: () => { },
}

interface User {
  id: string;
  name: string;
  firstName: string | null,
  lastName: string | null,
  email: string;
}

export const AuthContext = createContext<AuthContextType>(defaultAuthContext);

interface AuthProviderProps {
  children: ReactNode;
}

const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [accessToken, setAccessToken] = useState<string | null>(null);
  const [user, setUser] = useState<User | null>(null);

  const saveLogin = (accessToken: string, user: User) => {
    setAccessToken(accessToken);
    setUser(user);
  };

  const clearUser = () => {
    setAccessToken(null);
    setUser(null);
  };


  useEffect(() => {
    const autoLogin = async () => {
      if (!user) {
        const response = await fetch('http://localhost:5264/auth/generate-access-token', {
          method: 'POST',
          credentials: 'include', // Include the cookie with the request
          body: accessToken ?? "",
        });
        console.log(response);
        if (!response.ok) {
          console.log('Refresh token is invalid or expired');
          return
        }
        const data = await response.json();

        saveLogin(data.accessToken, data.user);
      };
    }

    autoLogin();
  }, []);

  return (
    <AuthContext.Provider value={{ accessToken, user, saveLogin, clearUser }}>
      {children}
    </AuthContext.Provider>
  );
};

// Custom Hook to access AuthContext
export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
};

export default AuthProvider;