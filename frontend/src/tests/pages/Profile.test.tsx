import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import Profile from '../../pages/Profile';
import { useAuth } from '../../hooks/useAuth';

jest.mock('../../hooks/useAuth');
const mockedUseAuth = useAuth as jest.MockedFunction<typeof useAuth>;

describe('Profile page', () => {
  beforeEach(() => {
    jest.resetAllMocks();
  });

  it('updates display name through the auth context', async () => {
    const updateDisplayName = jest.fn().mockResolvedValue(undefined);

    mockedUseAuth.mockReturnValue({
      user: {
        id: 'user-1',
        email: 'test@example.com',
        displayName: 'Old Name',
        role: 'User',
      },
      tokens: { accessToken: 'access', refreshToken: 'refresh', expiresIn: 900 },
      updateDisplayName,
    } as any);

    render(<Profile />);

    fireEvent.change(screen.getByLabelText(/display name/i), { target: { value: 'New Name' } });
    fireEvent.click(screen.getByRole('button', { name: /save display name/i }));

    await waitFor(() => expect(updateDisplayName).toHaveBeenCalledWith('New Name'));
    expect(await screen.findByText(/display name updated/i)).toBeInTheDocument();
  });
});