import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import Login from '../../pages/Login';
import { useAuth } from '../../hooks/useAuth';

jest.mock('../../hooks/useAuth');
const mockedUseAuth = useAuth as jest.MockedFunction<typeof useAuth>;

describe('Login page', () => {
  beforeEach(() => {
    jest.resetAllMocks();
  });

  it('submits the login form and calls login with entered credentials', async () => {
    const login = jest.fn().mockResolvedValue(undefined);
    const clearError = jest.fn();

    mockedUseAuth.mockReturnValue({
      login,
      loading: false,
      error: null,
      clearError,
    } as any);

    render(
      <MemoryRouter>
        <Login />
      </MemoryRouter>
    );

    fireEvent.change(screen.getByLabelText(/email/i), { target: { value: 'test@example.com' } });
    fireEvent.change(screen.getByLabelText(/password/i), { target: { value: 'password123' } });
    fireEvent.click(screen.getByRole('button', { name: /login/i }));

    await waitFor(() => {
      expect(login).toHaveBeenCalledWith('test@example.com', 'password123');
      expect(clearError).toHaveBeenCalled();
    });
  });

  it('renders an error message when login fails', async () => {
    const login = jest.fn().mockRejectedValue(new Error('Invalid credentials'));
    const clearError = jest.fn();

    mockedUseAuth.mockReturnValue({
      login,
      loading: false,
      error: 'Invalid credentials',
      clearError,
    } as any);

    render(
      <MemoryRouter>
        <Login />
      </MemoryRouter>
    );

    expect(screen.getByText(/invalid credentials/i)).toBeInTheDocument();
  });
});
