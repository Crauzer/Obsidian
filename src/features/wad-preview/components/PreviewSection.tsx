import { useWadContext } from "~/features/wad/providers";

export const PreviewSection = () => {
  const { currentPreviewItemId } = useWadContext();

  return <div className="h-full bg-gray-950">PreviewSection</div>;
};
