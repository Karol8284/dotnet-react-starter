import { FormEvent, useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { userApi } from '../../services/api';

export default function UpdateUser() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [address, setAddress] = useState('');
  const [loading, setLoading] = useState(false);
  const [initialLoading, setInitialLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let mounted = true;

    const loadUser = async () => {
      if (!id) {
        setInitialLoading(false);
        return;
      }

      try {
        const response = await userApi.getUser(id);
        if (mounted && response.data) {
          setFirstName(response.data.firstName);
          setLastName(response.data.lastName);
          setEmail(response.data.email);
          setPhoneNumber(response.data.phoneNumber);
          setAddress(response.data.address);
        }
      } catch (caughtError) {
        if (mounted) {
          setError(caughtError instanceof Error ? caughtError.message : 'Failed to load user');
        }
      } finally {
        if (mounted) {
          setInitialLoading(false);
        }
      }
    };

    void loadUser();

    return () => {
      mounted = false;
    };
  }, [id]);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!id) {
      return;
    }

    setLoading(true);
    setError(null);

    try {
      await userApi.updateUser(id, { firstName, lastName, email, phoneNumber, address });
      navigate('/users');
    } catch (caughtError) {
      setError(caughtError instanceof Error ? caughtError.message : 'Failed to update user');
    } finally {
      setLoading(false);
    }
  };

  if (initialLoading) {
    return <div className="page-state">Loading user...</div>;
  }

  return (
    <section className="page-shell form-shell">
      <h1>Update user</h1>
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
          {loading ? 'Saving...' : 'Save changes'}
        </button>
      </form>
    </section>
  );
}
