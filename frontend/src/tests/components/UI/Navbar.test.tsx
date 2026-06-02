import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { Navbar } from '../../../components/UI/Navbar';
import { useAuth } from '../../../hooks/useAuth';

jest.mock('../../../hooks/useAuth');
const mockedUseAuth = useAuth as jest.MockedFunction<typeof useAuth>;

const mockNavigate = jest.fn();
jest.mock('react-router-dom', () => {
  const actual = jest.requireActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

describe('Navbar', () => {
  beforeEach(() => {
    mockNavigate.mockClear();
    jest.resetAllMocks();
  });

  it('renders login and register links when user is not authenticated', () => {
    mockedUseAuth.mockReturnValue({
      isAuthenticated: false,
      user: null,
      logout: jest.fn(),
    } as any);

    render(
      <MemoryRouter>
        <Navbar />
      </MemoryRouter>
    );

    expect(screen.getByText(/login/i)).toBeInTheDocument();
    expect(screen.getByText(/register/i)).toBeInTheDocument();
  });

  it('shows the logged in user and calls logout when button is clicked', async () => {
    const logout = jest.fn().mockResolvedValue(undefined);

    mockedUseAuth.mockReturnValue({
      isAuthenticated: true,
      user: { displayName: 'Test User' },
      logout,
    } as any);

    render(
      <MemoryRouter>
        <Navbar />
      </MemoryRouter>
    );

    expect(screen.getByText(/test user/i)).toBeInTheDocument();
    const logoutButton = screen.getByRole('button', { name: /logout/i });

    fireEvent.click(logoutButton);

    await waitFor(() => expect(logout).toHaveBeenCalledTimes(1));
    expect(mockNavigate).toHaveBeenCalledWith('/');
  });
});
