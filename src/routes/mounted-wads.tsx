import { useSearchParams } from "react-router-dom";

import { Spinner } from "../components";
import { useMountedWads, WadDropZone, WadTabs } from "../features/wad";

export default function MountedWadItem() {
  const [searchParams, setSearchParams] = useSearchParams();
  const wadId = searchParams.get("wadId");

  const mountedWadsQuery = useMountedWads();

  const handleSelectedWadChanged = (selectedWad: string) => {
    setSearchParams({ wadId: selectedWad });
  };

  return (
    <div className="flex w-full px-2 py-2">
      {mountedWadsQuery.isLoading && <Spinner />}
      {mountedWadsQuery.isSuccess && (
        <>
          <WadDropZone />
          <WadTabs
            wads={mountedWadsQuery.data.wads}
            onSelectedWadChanged={handleSelectedWadChanged}
            selectedWad={wadId ?? undefined}
          />
        </>
      )}
    </div>
  );
}
