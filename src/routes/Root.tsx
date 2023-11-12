import { Outlet } from 'react-router-dom';

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
