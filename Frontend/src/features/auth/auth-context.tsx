import React, { createContext, useState, ReactNode, useEffect, useContext } from "react";

// Define types for AuthContext and its values
interface AuthContextType {
  authToken: string | null;
  user: User | null;
  saveLogin: (accessToken: string, user: User) => void;
  logout: () => void;
}

const defaultAuthContext: AuthContextType = {
  authToken: null,
  user: null,
  saveLogin: () => { },
  logout: () => { },
};

interface User {
  accessToken: string;
  name: string;
  email: string;
}

export const AuthContext = createContext<AuthContextType>(defaultAuthContext);

interface AuthProviderProps {
  children: ReactNode;
}

const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [authToken, setAuthToken] = useState<string | null>(null);
  const [user, setUser] = useState<User | null>(null);

  const saveLogin = (accessToken: string, user: User) => {
    setAuthToken(accessToken);
    setUser(user);
    // setUser(userData);
  };

  const logout = () => {
    setAuthToken(null);
    // setUser(null);
  };


  useEffect(() => {
    const checkLogin = async () => {
      console.log("useEffect called.");
      if (!user) {
        const response = await fetch('http://localhost:5264/get-user', {
          method: 'POST',
          credentials: 'include', // Include the cookie with the request
          body: authToken ?? "",
        });
        console.log(response);
        if (!response.ok) {
          console.log('Refresh token is invalid or expired');
          return
        }
        const data = await response.json();
        // Save the new access token in React Context or localStorage
        console.log(data + " trying to refresh.");
        saveLogin(data.accessToken, data.user);
      };
    }

    checkLogin();
  }, []);

  return (
    <AuthContext.Provider value={{ authToken, user, saveLogin, logout }}>
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