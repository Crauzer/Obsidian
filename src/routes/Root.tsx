import { Outlet } from 'react-router-dom';
import { toast } from 'react-toastify';

import { Button } from '../components';
import { Layout } from '../components/layout';

export default function Root() {
  return (
    <>
      <Layout>
        <Outlet />
      </Layout>
    </>
  );
}
