import { FormEvent, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { userApi } from '../../services/api';

export default function CreateUser() {
  const navigate = useNavigate();
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [address, setAddress] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setLoading(true);
    setError(null);

    try {
      await userApi.createUser({ firstName, lastName, email, phoneNumber, address });
      navigate('/users');
    } catch (caughtError) {
      setError(caughtError instanceof Error ? caughtError.message : 'Failed to create user');
    } finally {
      setLoading(false);
    }
  };

  return (
    <section className="page-shell form-shell">
      <h1>Create user</h1>
      <form className="form" onSubmit={handleSubmit}>
        <div className="grid grid--2">
          <label>
            First name
            <input value={firstName} onChange={(event) => setFirstName(event.target.value)} required />
          </label>
          <label>
            Last name
            <input value={lastName} onChange={(event) => setLastName(event.target.value)} required />
          </label>
        </div>
        <label>
          Email
          <input value={email} onChange={(event) => setEmail(event.target.value)} type="email" required />
        </label>
        <div className="grid grid--2">
          <label>
            Phone number
            <input value={phoneNumber} onChange={(event) => setPhoneNumber(event.target.value)} required />
          </label>
          <label>
            Address
            <input value={address} onChange={(event) => setAddress(event.target.value)} required />
          </label>
        </div>
        {error ? <p className="form__error">{error}</p> : null}
        <button className="button" type="submit" disabled={loading}>
          {loading ? 'Creating...' : 'Create user'}
        </button>
      </form>
    </section>
  );
}
