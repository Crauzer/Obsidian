import React from 'react';
import { createBrowserRouter } from 'react-router-dom';

const Root = React.lazy(() => import('../routes/Root'));
const MountedWads = React.lazy(() => import('../routes/MountedWads'));
const ComponentTest = React.lazy(() => import('../routes/ComponentTest'));

export const appRoutes = {
  root: '/' as const,
  explorer: '/explorer' as const,
  mountedWads: '/mountedWads' as const,
  wad: '/wad/:wadId' as const,
  componentTest: '/componentTest' as const,
};

export const router = createBrowserRouter([
  {
    path: appRoutes.root,
    element: <Root />,
    children: [
      { path: appRoutes.explorer },
      {
        path: appRoutes.mountedWads,
        element: <MountedWads />,
      },
    ],
  },
  { path: appRoutes.componentTest, element: <ComponentTest /> },
]);
