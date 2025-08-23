import { useEffect } from "react";
import { Outlet, useMatch, useNavigate } from "react-router-dom";

import { Layout } from "../components/layout";
import { appRoutes } from "../lib/router";

export default function Root() {
  const navigate = useNavigate();

  const rootMatch = useMatch(appRoutes.root);

  useEffect(() => {
    if (rootMatch) {
      navigate(appRoutes.mountedWads);
    }
  }, [navigate, rootMatch]);

  return (
    <>
      <Layout>
        <Outlet />
      </Layout>
    </>
  );
}
