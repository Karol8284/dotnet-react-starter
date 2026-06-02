import { Link } from 'react-router-dom';

export default function NotFound() {
  return (
    <section className="page-shell">
      <h1>404</h1>
      <p>Strona nie istnieje.</p>
      <Link className="button" to="/">
        Go home
      </Link>
    </section>
  );
}
