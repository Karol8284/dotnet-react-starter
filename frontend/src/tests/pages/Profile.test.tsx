import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import Profile from '../../pages/Profile';
import { useAuth } from '../../hooks/useAuth';

jest.mock('../../hooks/useAuth');
const mockedUseAuth = useAuth as jest.MockedFunction<typeof useAuth>;

describe('Profile page', () => {
  beforeEach(() => {
    jest.resetAllMocks();
  });

  it('updates profile details through the auth context', async () => {
    const updateProfile = jest.fn().mockResolvedValue(undefined);
    const changePassword = jest.fn().mockResolvedValue(undefined);

    mockedUseAuth.mockReturnValue({
      user: {
        id: 'user-1',
        email: 'test@example.com',
        displayName: 'Old Name',
        firstName: 'Old',
        lastName: 'Name',
        avatarUrl: '',
        role: 'User',
      },
      tokens: { accessToken: 'access', expiresIn: 900 },
      updateProfile,
      changePassword,
    } as any);

    render(<Profile />);

    fireEvent.change(screen.getByLabelText(/first name/i), { target: { value: 'New' } });
    fireEvent.change(screen.getByLabelText(/last name/i), { target: { value: 'Profile' } });
    fireEvent.change(screen.getByLabelText(/email/i), { target: { value: 'updated@example.com' } });
    fireEvent.change(screen.getByLabelText(/avatar url/i), { target: { value: 'https://example.com/avatar.png' } });
    fireEvent.click(screen.getByRole('button', { name: /save profile/i }));

    await waitFor(() => expect(updateProfile).toHaveBeenCalledWith({
      firstName: 'New',
      lastName: 'Profile',
      email: 'updated@example.com',
      avatarUrl: 'https://example.com/avatar.png',
    }));
    expect(await screen.findByText(/profile updated/i)).toBeInTheDocument();
  });

  it('changes password through the auth context', async () => {
    const updateProfile = jest.fn().mockResolvedValue(undefined);
    const changePassword = jest.fn().mockResolvedValue(undefined);

    mockedUseAuth.mockReturnValue({
      user: {
        id: 'user-1',
        email: 'test@example.com',
        displayName: 'Old Name',
        role: 'User',
      },
      tokens: { accessToken: 'access', expiresIn: 900 },
      updateProfile,
      changePassword,
    } as any);

    render(<Profile />);

    fireEvent.change(screen.getByLabelText(/^current password$/i), { target: { value: 'password123' } });
    fireEvent.change(screen.getByLabelText(/^new password$/i), { target: { value: 'newPassword123' } });
    fireEvent.change(screen.getByLabelText(/confirm new password/i), { target: { value: 'newPassword123' } });
    fireEvent.click(screen.getByRole('button', { name: /change password/i }));

    await waitFor(() => expect(changePassword).toHaveBeenCalledWith('password123', 'newPassword123'));
    expect(await screen.findByText(/password changed/i)).toBeInTheDocument();
  });
});