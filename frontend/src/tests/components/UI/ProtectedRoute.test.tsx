import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { ProtectedRoute } from '../../../components/UI/ProtectedRoute';
import { useAuth } from '../../../hooks/useAuth';

jest.mock('../../../hooks/useAuth');
const mockedUseAuth = useAuth as jest.MockedFunction<typeof useAuth>;

describe('ProtectedRoute', () => {
  beforeEach(() => {
    jest.resetAllMocks();
  });

  it('shows loading state while auth is loading', () => {
    mockedUseAuth.mockReturnValue({ isAuthenticated: false, loading: true } as any);

    render(
      <MemoryRouter initialEntries={['/dashboard']}>
        <Routes>
          <Route element={<ProtectedRoute />}> 
            <Route path="/dashboard" element={<div>Protected</div>} />
          </Route>
        </Routes>
      </MemoryRouter>
    );

    expect(screen.getByText(/loading session/i)).toBeInTheDocument();
  });

  it('redirects to login when not authenticated', () => {
    mockedUseAuth.mockReturnValue({ isAuthenticated: false, loading: false } as any);

    render(
      <MemoryRouter initialEntries={['/dashboard']}>
        <Routes>
          <Route path="/login" element={<div>Login Page</div>} />
          <Route element={<ProtectedRoute />}> 
            <Route path="/dashboard" element={<div>Protected</div>} />
          </Route>
        </Routes>
      </MemoryRouter>
    );

    expect(screen.getByText(/login page/i)).toBeInTheDocument();
  });

  it('renders protected content when authenticated', () => {
    mockedUseAuth.mockReturnValue({ isAuthenticated: true, loading: false } as any);

    render(
      <MemoryRouter initialEntries={['/dashboard']}>
        <Routes>
          <Route element={<ProtectedRoute />}> 
            <Route path="/dashboard" element={<div>Protected</div>} />
          </Route>
        </Routes>
      </MemoryRouter>
    );

    expect(screen.getByText(/protected/i)).toBeInTheDocument();
  });
});
