import { Outlet } from 'react-router-dom';

import { Toolbox } from '../components';
import { Layout } from '../components/layout';
import { env } from '../utils';

export default function Root() {
  return (
    <>
      <Layout>
        <Outlet />
      </Layout>
      {env.DEV && <Toolbox />}
    </>
  );
}
