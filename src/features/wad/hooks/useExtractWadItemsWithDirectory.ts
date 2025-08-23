import { useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { toast } from "react-toastify";

import { actionsQueryKeys } from "../../actions";
import { usePickDirectory } from "../../fs";
import { useSettings } from "../../settings";
import { useExtractWadItems } from "../api";

export type UseExtractWadItemsWithDirectoryContext = {
  wadId: string;
  parentItemId?: string;
  items: string[];
  actionId: string;
};

export const useExtractWadItemsWithDirectory = () => {
  const [t] = useTranslation("mountedWads");
  const queryClient = useQueryClient();

  const [isExtracting, setIsExtracting] = useState(false);

  const settings = useSettings({});

  const pickDirectory = usePickDirectory();
  const extractWadItems = useExtractWadItems();

  const extractWadItemsWithDirectory = ({
    wadId,
    parentItemId,
    items,
    actionId,
  }: UseExtractWadItemsWithDirectoryContext) => {
    setIsExtracting(true);

    pickDirectory.mutate(
      {
        initialDirectory:
          settings.data?.defaultExtractionDirectory ?? undefined,
      },
      {
        onSuccess: ({ path }) => {
          extractWadItems.mutate(
            {
              wadId,
              actionId,
              parentItemId,
              items,
              extractDirectory: path,
            },
            {
              onSuccess: () => {
                toast.success(t("extraction.success"));
              },
              onSettled: () => {
                setIsExtracting(false);
                queryClient.resetQueries({
                  queryKey: actionsQueryKeys.actionProgress("actionId"),
                });
              },
            },
          );
        },
        onError: (e) => {
          console.error(e);
          setIsExtracting(false);
        },
      },
    );
  };

  return { isExtracting, extractWadItemsWithDirectory };
};
