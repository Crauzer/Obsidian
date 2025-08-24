import { useEffect, useState } from "react";
import { useItemPreviewTypes } from "~/features/wad/api/getItemPreviewTypes";
import { useWadContext } from "~/features/wad/providers";
import { getImageBytes } from "../api";

export const PreviewSection = () => {
  const { wadId, currentPreviewItemId } = useWadContext();

  const { data: previewTypes } = useItemPreviewTypes({
    wadId,
    itemId: currentPreviewItemId ?? "",
    enabled: !!currentPreviewItemId,
  });

  const isImagePreviewType = previewTypes?.some((type) => type === "image");

  console.log(previewTypes);
  console.log(currentPreviewItemId);

  return (
    <div className="h-full bg-gray-950">
      {isImagePreviewType && currentPreviewItemId && (
        <ImagePreview wadId={wadId} itemId={currentPreviewItemId} />
      )}
    </div>
  );
};

const ImagePreview = ({ wadId, itemId }: { wadId: string; itemId: string }) => {
  const [src, setSrc] = useState<string | null>(null);

  useEffect(() => {
    const fetchImageBytes = async () => {
      const imageBytes = await getImageBytes(wadId, itemId);
      const blob = new Blob([imageBytes], { type: "image/png" });
      const url = URL.createObjectURL(blob);
      setSrc(url);
    };

    fetchImageBytes();
  }, [wadId, itemId]);

  return (
    <div>
      <img src={src ?? ""} alt="Preview" />
    </div>
  );
};
