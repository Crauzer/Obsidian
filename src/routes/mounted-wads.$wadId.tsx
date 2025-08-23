import {
  generatePath,
  useNavigate,
  useParams,
  useSearchParams,
} from "react-router-dom";

import { Spinner } from "../components";
import { useMountedWads, WadDropZone, WadTabs } from "../features/wad";
import { appRoutes } from "../lib/router";

export default function MountedWadItem() {
  const [searchParams] = useSearchParams();
  const { wadId } = useParams();
  const naviagte = useNavigate();

  const mountedWadsQuery = useMountedWads();

  const handleSelectedWadChanged = (selectedWad: string) => {
    naviagte(generatePath(appRoutes.mountedWad, { wadId: selectedWad }));
  };

  return (
    <div className="flex w-full px-2 py-2">
      {mountedWadsQuery.isLoading && <Spinner />}
      {mountedWadsQuery.isSuccess && (
        <>
          <WadDropZone />
          <WadTabs
            wads={mountedWadsQuery.data.wads}
            selectedWad={wadId}
            onSelectedWadChanged={handleSelectedWadChanged}
            selectedItemId={searchParams.get("itemId") ?? undefined}
          />
        </>
      )}
    </div>
  );
}
