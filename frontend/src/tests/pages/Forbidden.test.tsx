import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import Forbidden from '../../pages/Forbidden';

describe('Forbidden page', () => {
  it('shows the attempted route when navigation state is present', () => {
    render(
      <MemoryRouter initialEntries={[{ pathname: '/forbidden', state: { from: { pathname: '/admin' } } } as any]}>
        <Routes>
          <Route path="/forbidden" element={<Forbidden />} />
        </Routes>
      </MemoryRouter>
    );

    expect(screen.getByText(/requested route: \/admin/i)).toBeInTheDocument();
  });

  it('renders the generic forbidden message without a requested route fallback', () => {
    render(
      <MemoryRouter initialEntries={['/forbidden']}>
        <Routes>
          <Route path="/forbidden" element={<Forbidden />} />
        </Routes>
      </MemoryRouter>
    );

    expect(screen.getByText(/your current role does not allow access to this area/i)).toBeInTheDocument();
    expect(screen.queryByText(/requested route:/i)).not.toBeInTheDocument();
  });
});