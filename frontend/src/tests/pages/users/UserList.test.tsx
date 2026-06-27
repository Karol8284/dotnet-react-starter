import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import UserList from '../../../pages/users/UserList';
import { useAuth } from '../../../hooks/useAuth';
import { userApi } from '../../../services/api';
import type { UserDto } from '../../../types';

jest.mock('../../../hooks/useAuth');
jest.mock('../../../services/api', () => ({
  userApi: {
    getUsers: jest.fn(),
    deleteUser: jest.fn(),
    updateUserRole: jest.fn(),
  },
}));

const mockedUseAuth = useAuth as jest.MockedFunction<typeof useAuth>;
const mockedUserApi = userApi as jest.Mocked<typeof userApi>;

const createUser = (id: string, firstName: string): UserDto => ({
  id,
  firstName,
  lastName: 'Tester',
  email: `${firstName.toLowerCase()}@example.com`,
  phoneNumber: '+48 123 456 789',
  address: 'Warsaw',
  createdAt: '2026-06-26T00:00:00Z',
});

describe('UserList page', () => {
  beforeEach(() => {
    jest.resetAllMocks();
    mockedUseAuth.mockReturnValue({
      user: { id: 'current-user', displayName: 'Current User', email: 'current@example.com', role: 'User' },
    } as any);
  });

  it('loads users with default pagination and renders read-only state for regular users', async () => {
    mockedUserApi.getUsers.mockResolvedValue({
      statusCode: 200,
      message: 'OK',
      data: [createUser('user-1', 'Alice')],
      errors: null,
      timestamp: '2026-06-26T00:00:00Z',
    });

    render(<UserList />);

    expect(await screen.findByText(/alice tester/i)).toBeInTheDocument();
    expect(mockedUserApi.getUsers).toHaveBeenCalledWith(1, 10);
    expect(screen.getByText(/read-only/i)).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /delete/i })).not.toBeInTheDocument();
  });

  it('allows admins to request role changes', async () => {
    mockedUseAuth.mockReturnValue({
      user: { id: 'admin-user', displayName: 'Admin User', email: 'admin@example.com', role: 'Admin' },
    } as any);
    mockedUserApi.getUsers.mockResolvedValue({
      statusCode: 200,
      message: 'OK',
      data: [createUser('user-1', 'Alice')],
      errors: null,
      timestamp: '2026-06-26T00:00:00Z',
    });
    mockedUserApi.updateUserRole.mockResolvedValue({
      statusCode: 200,
      message: 'OK',
      data: createUser('user-1', 'Alice'),
      errors: null,
      timestamp: '2026-06-26T00:00:00Z',
    });

    render(<UserList />);

    await screen.findByText(/alice tester/i);
    fireEvent.click(screen.getByRole('button', { name: /set admin/i }));

    await waitFor(() => expect(mockedUserApi.updateUserRole).toHaveBeenCalledWith('user-1', 'Admin'));
    expect(await screen.findByText(/role update request saved as admin/i)).toBeInTheDocument();
  });

  it('requests the next page when pagination advances', async () => {
    const firstPageUsers = Array.from({ length: 10 }, (_, index) => createUser(`user-${index}`, `User${index}`));
    mockedUserApi.getUsers.mockResolvedValue({
      statusCode: 200,
      message: 'OK',
      data: firstPageUsers,
      errors: null,
      timestamp: '2026-06-26T00:00:00Z',
    });

    render(<UserList />);

    await screen.findByText(/user0 tester/i);
    fireEvent.click(screen.getByRole('button', { name: /next/i }));

    await waitFor(() => expect(mockedUserApi.getUsers).toHaveBeenCalledWith(2, 10));
  });
});