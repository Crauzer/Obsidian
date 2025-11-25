import React from "react";
import { createBrowserRouter } from "react-router-dom";

const Settings = React.lazy(() => import("../routes/Settings"));
const Root = React.lazy(() => import("../routes/Root"));
const MountedWads = React.lazy(() => import("../routes/mounted-wads"));
const MountedWad = React.lazy(() => import("../routes/mounted-wads.$wadId"));
const GameExplorer = React.lazy(() => import("../routes/game-explorer"));
const ComponentTest = React.lazy(() => import("../routes/ComponentTest"));

export const appRoutes = {
  componentTest: "/componentTest" as const,
  explorer: "/explorer" as const,
  gameExplorer: "/game-explorer" as const,
  mountedWads: "/mounted-wads" as const,
  mountedWad: "/mounted-wads/:wadId" as const,
  root: "/" as const,
  settings: "/settings" as const,
  wad: "/wad/:wadId" as const,
};

export const router = createBrowserRouter([
  {
    path: appRoutes.root,
    element: <Root />,

    children: [
      { path: appRoutes.explorer },
      {
        path: appRoutes.gameExplorer,
        element: <GameExplorer />,
      },
      {
        path: appRoutes.mountedWads,
        element: <MountedWads />,
      },
      { path: appRoutes.mountedWad, element: <MountedWad /> },
      { path: appRoutes.settings, element: <Settings /> },
    ],
  },
  { path: appRoutes.componentTest, element: <ComponentTest /> },
]);
