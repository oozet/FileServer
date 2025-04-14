import './App.css'
import AuthProvider from './context/auth-context'
import { Header } from './features/header/header'
import { Main } from './features/main/main'


function App() {

  return (
    <>
      <div className="layout">
        <AuthProvider>
          <Header />
          <Main />
        </AuthProvider>
      </div>
    </>
  )
}

export default App
