import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import AdminPanel from '../../pages/AdminPanel';
import { userApi } from '../../services/api';

jest.mock('../../services/api', () => ({
  userApi: {
    getUserCount: jest.fn(),
  },
}));

const mockedUserApi = userApi as jest.Mocked<typeof userApi>;

describe('AdminPanel page', () => {
  beforeEach(() => {
    jest.resetAllMocks();
  });

  it('renders the backend user count', async () => {
    mockedUserApi.getUserCount.mockResolvedValue({
      statusCode: 200,
      message: 'OK',
      data: 42,
      errors: null,
      timestamp: '2026-06-26T00:00:00Z',
    });

    render(
      <MemoryRouter>
        <AdminPanel />
      </MemoryRouter>
    );

    expect(await screen.findByRole('heading', { name: '42' })).toBeInTheDocument();
    expect(screen.getByText(/live count from the backend admin endpoint/i)).toBeInTheDocument();
  });

  it('renders an error state when the admin overview fails', async () => {
    mockedUserApi.getUserCount.mockRejectedValue(new Error('Access denied'));

    render(
      <MemoryRouter>
        <AdminPanel />
      </MemoryRouter>
    );

    expect(await screen.findByText(/access denied/i)).toBeInTheDocument();
  });

  it('renders a 403 API message when the admin overview is forbidden', async () => {
    mockedUserApi.getUserCount.mockRejectedValue(new Error('Forbidden'));

    render(
      <MemoryRouter>
        <AdminPanel />
      </MemoryRouter>
    );

    expect(await screen.findByText(/forbidden/i)).toBeInTheDocument();
  });

  it('falls back to a generic message for non-Error failures', async () => {
    mockedUserApi.getUserCount.mockRejectedValue('unexpected failure');

    render(
      <MemoryRouter>
        <AdminPanel />
      </MemoryRouter>
    );

    expect(await screen.findByText(/failed to load admin overview/i)).toBeInTheDocument();
  });
});